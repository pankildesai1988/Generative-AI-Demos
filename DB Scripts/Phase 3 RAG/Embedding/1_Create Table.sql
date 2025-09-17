CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE embeddings (
    embedding_id UUID PRIMARY KEY,
    chunk_id UUID NOT NULL,
    vector VECTOR(1536) NOT NULL,
    model VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);
