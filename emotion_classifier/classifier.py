import pytorch_lightning as pl
import textdistance as td
import numpy as np
import pandas as pd
import argparse
import torch
from torch import nn
import torch.nn.functional as F
import re
import nltk
nltk.download("stopwords")
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
    self.hparams = hparams

    self.model = T5ForConditionalGeneration.from_pretrained(hparams.model_name_or_path)
    self.tokenizer = T5Tokenizer.from_pretrained(hparams.tokenizer_name_or_path)

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
    emo_model.load_state_dict(torch.load('T5_small_emotion.pt', map_location=torch.device('cpu')))

def get_emotion(text):
  '''
  Computes and returns an emotion label given an utterance
  '''
  text = re.sub(r'[^\w\s]', '', text)
  text = text.lower()
  with torch.no_grad():
      input_ids = emo_model.tokenizer.encode(text + '</s>', return_tensors='pt')
      output = emo_model.model.generate(input_ids=input_ids, max_length=2)
      dec = [emo_model.tokenizer.decode(ids) for ids in output]
  label = dec[0]
  return label