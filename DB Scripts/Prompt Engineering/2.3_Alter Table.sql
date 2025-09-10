ALTER TABLE [dbo].[PromptTemplates]
ADD [Version] INT NOT NULL DEFAULT(1);

ALTER TABLE [dbo].[PromptTemplates]
ADD [IsActive] BIT NOT NULL DEFAULT(1);

ALTER TABLE [dbo].[PromptTemplates]
ADD [UpdatedAt] DATETIME2 NULL;

ALTER TABLE [dbo].[PromptTemplateParameters]
ALTER COLUMN [Options] NVARCHAR(MAX) NULL;

CREATE TABLE [dbo].[PromptTemplateVersions](
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [TemplateId] INT NOT NULL,
    [Version] INT NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [KeyName] NVARCHAR(50) NOT NULL,
    [TemplateText] NVARCHAR(MAX) NOT NULL,
    [ParametersJson] NVARCHAR(MAX) NULL, -- Store params as JSON snapshot
    [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE())
);

ALTER TABLE [dbo].[PromptTemplateVersions]
ADD CONSTRAINT FK_PromptTemplateVersions_Templates FOREIGN KEY(TemplateId)
REFERENCES [dbo].[PromptTemplates](Id)
ON DELETE CASCADE;


CREATE TABLE AdminUsers (
    Id INT IDENTITY PRIMARY KEY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(200) NOT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'Admin',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Example: store SHA256 hash of password "Admin@123"
INSERT INTO AdminUsers (Username, PasswordHash, Role) 
VALUES ('admin', 'e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7', 'Admin');


