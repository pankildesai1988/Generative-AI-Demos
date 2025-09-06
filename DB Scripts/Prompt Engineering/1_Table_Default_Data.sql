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
