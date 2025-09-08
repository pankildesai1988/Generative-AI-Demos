CREATE TABLE ChatSessions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId NVARCHAR(100) NOT NULL DEFAULT 'default',
    Title NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE ChatMessages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ChatSessionId INT NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_ChatMessages_ChatSessions FOREIGN KEY (ChatSessionId)
        REFERENCES ChatSessions(Id) ON DELETE CASCADE
);

CREATE INDEX IX_ChatMessages_ChatSessionId ON ChatMessages(ChatSessionId);


ALTER TABLE ChatSessions
ADD Model NVARCHAR(50) NULL;

ALTER TABLE [dbo].[ChatSessions] 
ADD [LastMessageAt] DATETIME2 DEFAULT (getutcdate())