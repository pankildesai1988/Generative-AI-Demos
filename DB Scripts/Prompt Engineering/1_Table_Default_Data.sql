CREATE TABLE PromptTemplates (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    KeyName NVARCHAR(50) NOT NULL UNIQUE,
    TemplateText NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Insert Prompt Templates into the database
INSERT INTO PromptTemplates (Name, KeyName, TemplateText, CreatedAt)
VALUES 
  ('Freeform (Custom)', 'freeform', '{input}', GETUTCDATE()),

  ('Product Description Writer', 'product', 
   'You are a marketing copywriter. Write a short, engaging product description.\nProduct: {input}\nDescription:', 
   GETUTCDATE()),

  ('Summarizer', 'summary', 
   'Summarize the following text in 3 sentences:\n\n{input}\n\nSummary:', 
   GETUTCDATE()),

  ('Q&A Assistant', 'qa', 
   'You are a helpful assistant. Answer the following question clearly and concisely:\nQuestion: {input}\nAnswer:', 
   GETUTCDATE());

CREATE UNIQUE INDEX IX_PromptTemplates_KeyName
ON PromptTemplates (KeyName);

CREATE TABLE PromptTemplateParameters (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TemplateId INT NOT NULL FOREIGN KEY REFERENCES PromptTemplates(Id) ON DELETE CASCADE,
    Name NVARCHAR(50) NOT NULL,         -- e.g., Tone
    KeyName NVARCHAR(50) NOT NULL,      -- e.g., tone
    Options NVARCHAR(500) NULL,         -- e.g., "casual,professional,funny"
    DefaultValue NVARCHAR(100) NULL,    -- e.g., "professional"
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);


-- Add parameters for Product Description Writer
INSERT INTO PromptTemplateParameters (TemplateId, Name, KeyName, Options, DefaultValue)
VALUES 
  (2, 'Tone', 'tone', 'casual,professional,funny', 'professional'),
  (2, 'Length', 'length', 'short,medium,long', 'short');

-- Add parameter for Summarizer
INSERT INTO PromptTemplateParameters (TemplateId, Name, KeyName, Options, DefaultValue)
VALUES 
  (3, 'Length', 'length', 'short,medium,long', 'medium');
