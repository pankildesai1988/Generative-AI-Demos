-- Enable Full-Text Search if not already installed
EXEC sp_fulltext_database 'enable';

-- Create a full-text catalog (if none exists)
CREATE FULLTEXT CATALOG DocumentChunksFTS;

-- Create full-text index on DocumentChunks.Text
CREATE FULLTEXT INDEX ON DocumentChunks(Text)
    KEY INDEX PK_DocumentChunks -- your PK index name
    ON DocumentChunksFTS;
