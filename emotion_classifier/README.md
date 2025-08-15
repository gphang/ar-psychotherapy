# Emotion Recognition Model

To run this emotion recognition model, download the RoBERTa model through https://drive.google.com/file/d/1cycrYd0S3Go7j3W2A50bJKQBn-oCdgZs/view?usp=sharing, then place the downloaded model into the emotion_classifier folder.

Alternatively, you could use a smaller-sized T5 model instead (T5_small_emotion.pt): https://drive.google.com/drive/folders/19uh2aIyxqzS-4LiagBouu6YuPYiOvaDz

Example terminal line commands you can try running for local testing of model:
```
python app.py
curl -X POST -H "Content-Type: application/json" -d '{"text": "im happy"}' http://0.0.0.0:5001/evaluate
```


## Cloud Deployment
After setting up your gcloud CLI, run the following to build the Docker image, push, and deploy to Cloud Run:
```
gcloud builds submit --tag gcr.io/$PROJECT_ID/$IMAGE_NAME
gcloud run deploy $IMAGE_NAME \
  --image gcr.io/$PROJECT_ID/$IMAGE_NAME \
  --platform managed \
  --region $REGION \
  --memory 2Gi \
  --allow-unauthenticated
```