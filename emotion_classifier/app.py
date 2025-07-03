# import pytorch_lightning as pl
# import argparse
# import torch
# import re

# from transformers import (
#     T5ForConditionalGeneration,
#     T5Tokenizer
# )
# from tokenizers import ByteLevelBPETokenizer

# from tokenizers.processors import BertProcessing


# class T5FineTuner(pl.LightningModule):
#   def __init__(self, hparams):
#     super(T5FineTuner, self).__init__()
#     self.config = hparams

#     self.model = T5ForConditionalGeneration.from_pretrained(self.config.model_name_or_path)
#     self.tokenizer = T5Tokenizer.from_pretrained(self.config.tokenizer_name_or_path)

#   def forward(
#       self, input_ids, attention_mask=None, decoder_input_ids=None, decoder_attention_mask=None, lm_labels=None
#   ):
#     return self.model(
#         input_ids,
#         attention_mask=attention_mask,
#         decoder_input_ids=decoder_input_ids,
#         decoder_attention_mask=decoder_attention_mask,
#         lm_labels=lm_labels,
#     )

# args_dict = dict(
#     model_name_or_path='t5-small',
#     tokenizer_name_or_path='t5-small',
# )

# args = argparse.Namespace(**args_dict)


# #load emotion classifier (T5 small)
# with torch.no_grad():
#     emo_model = T5FineTuner(args)
#     emo_model.load_state_dict(torch.load('T5_small_emotion.pt', map_location=torch.device('cpu')), strict=False)

# def get_emotion(text):
#   '''
#   Computes and returns an emotion label given an utterance
#   '''
#   text = re.sub(r'[^\w\s]', '', text)
#   text = text.lower()
#   with torch.no_grad():
#       input_ids = emo_model.tokenizer.encode(text + '</s>', return_tensors='pt')
#       output = emo_model.model.generate(input_ids=input_ids, max_length=2)
#       dec = [emo_model.tokenizer.decode(ids, skip_special_tokens=True) for ids in output]
#   label = dec[0].strip()
#   return label






import re
import torch
from torch import nn
from transformers import AutoModelForMaskedLM
from tokenizers import ByteLevelBPETokenizer
from tokenizers.processors import BertProcessing


emotions = ['fear', 'love', 'instability', 'disgust', 'disappointment',
          'shame', 'anger', 'jealous', 'sadness', 'envy', 'joy', 'guilt']
emotion2int = dict(zip(emotions, list(range(len(emotions)))))

labels = ['not_s', 's']
label2int = dict(zip(labels, list(range(len(labels)))))

class ClassificationModel(nn.Module):
    def __init__(self, base_model, n_classes, base_model_output_size=768, dropout=0.05):
        super().__init__()
        self.base_model = base_model
        
        self.classifier = nn.Sequential(
            nn.Dropout(dropout),
            nn.Linear(base_model_output_size, base_model_output_size),
            nn.Mish(),
            nn.Dropout(dropout),
            nn.Linear(base_model_output_size, n_classes)
        )
        
        for layer in self.classifier:
            if isinstance(layer, nn.Linear):
                layer.weight.data.normal_(mean=0.0, std=0.02)
                if layer.bias is not None:
                    layer.bias.data.zero_()

    def forward(self, input_):
        X, attention_mask = input_
        hidden_states = self.base_model(X, attention_mask=attention_mask)
        
        return self.classifier(hidden_states[0][:, 0, :])

with torch.no_grad():
    emo_model = ClassificationModel(AutoModelForMaskedLM.from_pretrained("roberta-base").base_model, len(emotions))
    emo_model.load_state_dict(torch.load('RoBERTa_12_emotions.pt', map_location=torch.device('cpu')), strict=False)

def get_classification(text):
    text = re.sub(r'[^\w\s]', '', text)
    text = text.lower()

    t = ByteLevelBPETokenizer(
                "tokenizer/vocab.json",
                "tokenizer/merges.txt"
            )
    t._tokenizer.post_processor = BertProcessing(
                ("</s>", t.token_to_id("</s>")),
                ("<s>", t.token_to_id("<s>")),
            )
    t.enable_truncation(512)
    t.enable_padding(pad_id=t.token_to_id("<pad>"))
    tokenizer = t

    encoded = tokenizer.encode(text)
    sequence_padded = torch.tensor(encoded.ids).unsqueeze(0)
    attention_mask_padded = torch.tensor(encoded.attention_mask).unsqueeze(0)
   
    output = emo_model((sequence_padded, attention_mask_padded))
    _, top_class = output.topk(1, dim=1)
    label = int(top_class[0][0])
    label_map = {v: k for k, v in emotion2int.items()}

    return label_map[label]


# --- [Flask Web Server Logic] ---
from flask import Flask, request, jsonify

app = Flask(__name__)

@app.route('/evaluate', methods=['POST'])
def evaluate():
    data = request.get_json()
    if not data or 'text' not in data:
        return jsonify({'error': 'No text provided'}), 400

    user_text = data['text']
    # emotion = get_emotion(user_text)
    emotion = get_classification(user_text)
    
    print(f"Received: '{user_text}', Predicted: '{emotion}'") # Server-side log
    
    return jsonify({'emotion': emotion})

if __name__ == '__main__':
    # Runs the server on your local machine on port 5000
    app.run(host='0.0.0.0', port=5001)