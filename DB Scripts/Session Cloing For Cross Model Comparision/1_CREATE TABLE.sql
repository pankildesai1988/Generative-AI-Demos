-- SessionComparisons table
CREATE TABLE SessionComparisons (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OriginalSessionId INT NOT NULL,
    InputText NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- ComparisonResults table (normalized with Provider + ModelName)
CREATE TABLE ComparisonResults (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SessionComparisonId INT NOT NULL FOREIGN KEY REFERENCES SessionComparisons(Id) ON DELETE CASCADE,
    Provider NVARCHAR(100) NOT NULL,      -- e.g. "OpenAI", "Claude-3", "Gemini-1.5"
    ModelName NVARCHAR(100) NOT NULL,     -- e.g. "gpt-4o-mini", "opus", "pro"
    ResponseText NVARCHAR(MAX),
    LatencyMs FLOAT NULL,
    RawResponse NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Allow ResponseText to be NULL
ALTER TABLE ComparisonResults ALTER COLUMN ResponseText NVARCHAR(MAX) NULL;

-- Allow RawResponse to be NULL
ALTER TABLE ComparisonResults ALTER COLUMN RawResponse NVARCHAR(MAX) NULL;

-- Add ErrorCode column (nullable, max 50 chars)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'ErrorCode' AND Object_ID = Object_ID(N'ComparisonResults')
)
BEGIN
    ALTER TABLE ComparisonResults ADD ErrorCode NVARCHAR(50) NULL;
END

-- Add ErrorMessage column (nullable, max 500 chars)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'ErrorMessage' AND Object_ID = Object_ID(N'ComparisonResults')
)
BEGIN
    ALTER TABLE ComparisonResults ADD ErrorMessage NVARCHAR(500) NULL;
END



Select * from SessionComparisons
Select * from ComparisonResults

