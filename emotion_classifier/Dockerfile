FROM python:3.13-slim

# Set the working directory inside the container
WORKDIR /app

COPY requirements.txt .
RUN apt-get update && apt-get install -y --no-install-recommends build-essential cmake \
    && pip install --no-cache-dir -r requirements.txt \
    && apt-get purge -y --auto-remove build-essential cmake \
    && rm -rf /var/lib/apt/lists/*

COPY RoBERTa_12_emotions.pt .
COPY tokenizer/ ./tokenizer/
COPY app.py .

# Google Cloud Run uses 8080 by default
EXPOSE 8080

# Command to run the application using Gunicorn
# --timeout 0 disables the timeout, which is useful for long-running model inferences.
# --workers 1 is a safe bet for a memory-intensive ML model.
CMD ["gunicorn", "--bind", "0.0.0.0:8080", "--workers", "1", "--timeout", "0", "app:app"]