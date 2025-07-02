# # All your existing imports (torch, T5, etc.) go here
# import pytorch_lightning as pl
# import argparse
# import torch
# import re
# from transformers import T5ForConditionalGeneration, T5Tokenizer

# # --- [Your T5FineTuner class definition here] ---
# class T5FineTuner(pl.LightningModule):
#   def __init__(self, hparams):
#     super(T5FineTuner, self).__init__()
#     # This is an abbreviated version, use your full class
#     self.model = T5ForConditionalGeneration.from_pretrained(hparams.model_name_or_path)
#     self.tokenizer = T5Tokenizer.from_pretrained(hparams.tokenizer_name_or_path)

#   def forward(self, input_ids, **kwargs):
#     return self.model(input_ids, **kwargs)

# # --- [Model Loading - This only runs once on server start] ---
# print("Loading model and tokenizer...")
# args_dict = dict(model_name_or_path='t5-small', tokenizer_name_or_path='t5-small')
# args = argparse.Namespace(**args_dict)
# emo_model = T5FineTuner(args)
# # Ensure your 'T5_small_emotion.pt' file is in the same directory
# emo_model.load_state_dict(torch.load('T5_small_emotion.pt', map_location=torch.device('cpu')))
# emo_model.eval() # Set model to evaluation mode
# print("Model loaded successfully.")


# # --- [Your get_emotion function] ---
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


import pytorch_lightning as pl
import argparse
import torch
import re
#from nltk.corpus import stopwords

from transformers import (
    T5ForConditionalGeneration,
    T5Tokenizer
)
from tokenizers import ByteLevelBPETokenizer

from tokenizers.processors import BertProcessing

#T5:
class T5FineTuner(pl.LightningModule):
  def __init__(self, hparams):
    super(T5FineTuner, self).__init__()
    self.config = hparams

    self.model = T5ForConditionalGeneration.from_pretrained(self.config.model_name_or_path)
    self.tokenizer = T5Tokenizer.from_pretrained(self.config.tokenizer_name_or_path)

  def forward(
      self, input_ids, attention_mask=None, decoder_input_ids=None, decoder_attention_mask=None, lm_labels=None
  ):
    return self.model(
        input_ids,
        attention_mask=attention_mask,
        decoder_input_ids=decoder_input_ids,
        decoder_attention_mask=decoder_attention_mask,
        lm_labels=lm_labels,
    )

args_dict = dict(
    model_name_or_path='t5-small',
    tokenizer_name_or_path='t5-small',
)

args = argparse.Namespace(**args_dict)


#load emotion classifier (T5 small)
with torch.no_grad():
    emo_model = T5FineTuner(args)
    emo_model.load_state_dict(torch.load('T5_small_emotion.pt', map_location=torch.device('cpu')), strict=False)

def get_emotion(text):
  '''
  Computes and returns an emotion label given an utterance
  '''
  text = re.sub(r'[^\w\s]', '', text)
  text = text.lower()
  with torch.no_grad():
      input_ids = emo_model.tokenizer.encode(text + '</s>', return_tensors='pt')
      output = emo_model.model.generate(input_ids=input_ids, max_length=2)
      dec = [emo_model.tokenizer.decode(ids, skip_special_tokens=True) for ids in output]
  label = dec[0].strip()
  return label

# --- [Flask Web Server Logic] ---
from flask import Flask, request, jsonify

app = Flask(__name__)

@app.route('/evaluate', methods=['POST'])
def evaluate():
    data = request.get_json()
    if not data or 'text' not in data:
        return jsonify({'error': 'No text provided'}), 400

    user_text = data['text']
    emotion = get_emotion(user_text)
    
    print(f"Received: '{user_text}', Predicted: '{emotion}'") # Server-side log
    
    return jsonify({'emotion': emotion})

if __name__ == '__main__':
    # Runs the server on your local machine on port 5000
    app.run(host='0.0.0.0', port=5001)