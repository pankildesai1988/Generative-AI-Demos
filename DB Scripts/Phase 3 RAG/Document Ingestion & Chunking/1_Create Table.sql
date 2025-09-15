CREATE TABLE Documents (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Type NVARCHAR(50) NOT NULL,       -- pdf, docx, md, sql
    Metadata NVARCHAR(MAX) NULL,      -- optional JSON
    Version INT NOT NULL DEFAULT 1,
    UploadedBy NVARCHAR(100) NOT NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);


CREATE TABLE DocumentChunks (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DocumentId INT NOT NULL,
    ChunkOrder INT NOT NULL,
    Text NVARCHAR(MAX) NOT NULL,
    CONSTRAINT FK_DocumentChunks_Documents FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE
);
