
CREATE TABLE core.UserEngineRules (
    RuleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rules PRIMARY KEY,
    UserId NVARCHAR(16) NOT NULL,
    RuleName NVARCHAR(200) NOT NULL,
    Priority INT NOT NULL CONSTRAINT DF_Rules_Priority DEFAULT (100),
    IsActive BIT NOT NULL CONSTRAINT DF_Rules_IsActive DEFAULT (1),
);
GO

CREATE TABLE core.UserEngineTriggers (
    TriggerId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Triggers PRIMARY KEY,
    RuleId INT NOT NULL,
    FieldName NVARCHAR(100) NOT NULL,
    Operator NVARCHAR(20) NOT NULL,
    Value NVARCHAR(500) NOT NULL
);
GO

CREATE TABLE core.UserEngineConditionGroups (
    ConditionGroupId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConditionGroups PRIMARY KEY,
    RuleId INT NOT NULL,
    ParentConditionGroupId INT NULL,
    LogicOperator VARCHAR(3) NOT NULL
);
GO

CREATE TABLE core.UserEngineConditions (
    ConditionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Conditions PRIMARY KEY,
    ConditionGroupId INT NOT NULL,
    FieldName NVARCHAR(100) NOT NULL,
    Operator NVARCHAR(20) NOT NULL,
    Value NVARCHAR(500) NOT NULL
);
GO

CREATE TABLE core.UserEngineFieldDefinitions (
    FieldId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FieldDefinitions PRIMARY KEY,
    FieldName NVARCHAR(100) NOT NULL CONSTRAINT UQ_FieldDefinitions_FieldName UNIQUE,
    DataType VARCHAR(20) NOT NULL,
    AllowedOperatorsJson NVARCHAR(MAX) NOT NULL
);
GO

CREATE TABLE core.UserEngineActions (
    ActionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Actions PRIMARY KEY,
    RuleId INT NOT NULL,
    ActionType NVARCHAR(100) NOT NULL,
    ActionValue NVARCHAR(1000) NULL
);
GO

CREATE TABLE core.UserEngineTransactionEvents (
    EventId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TransactionEvents PRIMARY KEY,
    TransactionOccurred BIT NOT NULL,
    AccountId INT NOT NULL,
    TransactionId BIGINT NOT NULL,
    Amount DECIMAL(19,4) NOT NULL,
    AvailableBalance DECIMAL(19,4) NOT NULL,
    MerchantName NVARCHAR(250) NULL,
    MerchantType NVARCHAR(100) NULL,
    TransactionType NVARCHAR(100) NOT NULL,
    TransactionDateUtc DATETIME2(3) NOT NULL
);
GO

CREATE TABLE core.UserEngineRuleEvaluations (
    EvaluationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RuleEvaluations PRIMARY KEY,
    EventId INT NOT NULL,
    RuleId INT NOT NULL,
    Matched BIT NOT NULL,
    EvaluationDateUtc DATETIME2(3) NOT NULL CONSTRAINT DF_RuleEvaluations_Date DEFAULT (SYSUTCDATETIME())
);
GO

CREATE TABLE core.UserEngineConditionEvaluations (
    ConditionEvaluationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConditionEvaluations PRIMARY KEY,
    EvaluationId INT NOT NULL,
    ConditionId INT NOT NULL,
    ActualValue NVARCHAR(500) NULL,
    ExpectedValue NVARCHAR(500) NULL,
    Result BIT NOT NULL
);
GO

CREATE TABLE core.UserEngineActionExecutions (
    ActionExecutionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ActionExecutions PRIMARY KEY,
    EvaluationId INT NOT NULL,
    ActionId INT NOT NULL,
    Status VARCHAR(20) NOT NULL,
    ExecutionDateUtc DATETIME2(3) NOT NULL CONSTRAINT DF_ActionExecutions_Date DEFAULT (SYSUTCDATETIME())
);
GO

CREATE INDEX IX_Rules_UserId_IsActive ON core.UserEngineRules(UserId, IsActive);
CREATE INDEX IX_Triggers_RuleId ON core.UserEngineTriggers(RuleId);
CREATE INDEX IX_ConditionGroups_RuleId ON core.UserEngineConditionGroups(RuleId);
CREATE INDEX IX_Conditions_ConditionGroupId ON core.UserEngineConditions(ConditionGroupId);
CREATE INDEX IX_Actions_RuleId ON core.UserEngineActions(RuleId);
CREATE INDEX IX_TransactionEvents_AccountId_Date ON core.UserEngineTransactionEvents(AccountId, TransactionDateUtc);
GO