USE [DeveloperDynamic];
GO

SET XACT_ABORT ON;
SET NOCOUNT ON;

/*
    Realistic sample transactions for AccountID 21086
    Date range: 2026-09-16 through 2026-09-24
      - Past 7 days from today (2026-09-23)
      - Includes today and tomorrow (2026-09-24)

    Assumed opening balance before first transaction: $4,247.83
    Amounts are positive; Debit = 1 for debits, Debit = 0 for credits.

    Review before running:
      - Confirm AccountID 21086 exists in core.Account
      - Adjust CoreSequence values if they collide with existing rows
      - Remove or comment out the optional cleanup block below if re-running
*/

-- Optional: remove prior test rows for this account in the target window
/*
DELETE FROM [core].[Transactions]
WHERE [AccountID] = 21086
  AND [PostingDate] >= '2026-09-16'
  AND [PostingDate] <  '2026-09-25'
  AND [CoreSequence] BETWEEN 92609160001 AND 92609249999;
*/

BEGIN TRAN;

INSERT INTO [core].[Transactions] (
    [AccountID],
    [PostingDate],
    [EffectiveDate],
    [Amount],
    [Balance],
    [BatchNumber],
    [CheckNumber],
    [TransactionCode],
    [GeneralDescription],
    [SpecificDescription],
    [CoreSequence],
    [PostingSequence],
    [Debit],
    [MerchantClassificationCode],
    [TraceNumber],
    [IsVoid],
    [CreateDate]
)
VALUES
    -- 2026-09-16 (Tuesday)
    (21086, '2026-09-16 07:42:15', '2026-09-16', 5.47,  4242.36, 100216, NULL,   'POS', 'STARBUCKS STORE #2847',           'Debit Card Purchase - AUSTIN TX',         92609160001, 1001, 1, 5814, 'TRC240916001', 0, '2026-09-16 07:42:15'),
    (21086, '2026-09-16 12:18:44', '2026-09-16', 52.18, 4190.18, 100216, NULL,   'POS', 'SHELL OIL 57442180001',           'Debit Card Purchase - Gas',               92609160002, 1002, 1, 5541, 'TRC240916002', 0, '2026-09-16 12:18:44'),
    (21086, '2026-09-16 18:03:29', '2026-09-16', 127.43,4062.75, 100216, NULL,   'POS', 'KROGER #456',                     'Debit Card Purchase - Groceries',         92609160003, 1003, 1, 5411, 'TRC240916003', 0, '2026-09-16 18:03:29'),
    (21086, '2026-09-16 20:11:08', '2026-09-16', 35.00, 4097.75, 100217, NULL,   'ACH', 'VENMO CASHOUT',                   'P2P Transfer Received',                   92609160004, 1004, 0, NULL, 'TRC240916004', 0, '2026-09-16 20:11:08'),

    -- 2026-09-17 (Wednesday)
    (21086, '2026-09-17 06:01:00', '2026-09-17', 15.99, 4081.76, 100317, NULL,   'ACH', 'NETFLIX.COM',                     'Subscription - Monthly',                  92609170001, 1005, 1, 4899, 'TRC240917001', 0, '2026-09-17 06:01:00'),
    (21086, '2026-09-17 14:27:33', '2026-09-17', 23.67, 4058.09, 100317, NULL,   'POS', 'WALGREENS #12840',                'Debit Card Purchase - Pharmacy',          92609170002, 1006, 1, 5912, 'TRC240917002', 0, '2026-09-17 14:27:33'),
    (21086, '2026-09-17 19:45:12', '2026-09-17', 14.25, 4043.84, 100317, NULL,   'POS', 'CHIPOTLE 1847',                   'Debit Card Purchase - Dining',            92609170003, 1007, 1, 5812, 'TRC240917003', 0, '2026-09-17 19:45:12'),

    -- 2026-09-18 (Thursday) - payday
    (21086, '2026-09-18 06:00:00', '2026-09-18', 2847.62,6891.46, 100418, NULL,   'DD',  'ACME CORP PAYROLL',               'Direct Deposit - Payroll',                92609180001, 1008, 0, NULL, 'TRC240918001', 0, '2026-09-18 06:00:00'),
    (21086, '2026-09-18 08:15:22', '2026-09-18', 142.88,6748.58, 100418, NULL,   'ACH', 'CITY POWER & LIGHT',              'Utility Payment - Electric',              92609180002, 1009, 1, 4900, 'TRC240918002', 0, '2026-09-18 08:15:22'),
    (21086, '2026-09-18 21:33:51', '2026-09-18', 67.99, 6680.59, 100418, NULL,   'POS', 'AMZN Mktp US*2K9F3A',             'Debit Card Purchase - Online Retail',     92609180003, 1010, 1, 5399, 'TRC240918003', 0, '2026-09-18 21:33:51'),

    -- 2026-09-19 (Friday)
    (21086, '2026-09-19 11:02:18', '2026-09-19', 89.34, 6591.25, 100519, NULL,   'POS', 'TARGET T-2847',                   'Debit Card Purchase - General Merch',     92609190001, 1011, 1, 5310, 'TRC240919001', 0, '2026-09-19 11:02:18'),
    (21086, '2026-09-19 19:28:44', '2026-09-19', 78.42, 6512.83, 100519, NULL,   'POS', 'OLIVE GARDEN 0001234',            'Debit Card Purchase - Dining',            92609190002, 1012, 1, 5812, 'TRC240919002', 0, '2026-09-19 19:28:44'),
    (21086, '2026-09-19 20:55:03', '2026-09-19', 100.00,6412.83, 100519, NULL,   'ATM', 'ATM WITHDRAWAL',                  'ATM Cash Withdrawal - Branch ATM',        92609190003, 1013, 1, 6011, 'TRC240919003', 0, '2026-09-19 20:55:03'),
    (21086, '2026-09-19 23:59:59', '2026-09-19', 0.12,  6412.95, 100519, NULL,   'INT', 'INTEREST PAYMENT',                'Monthly Interest Credit',                 92609190004, 1014, 0, NULL, 'TRC240919004', 0, '2026-09-19 23:59:59'),

    -- 2026-09-20 (Saturday) - includes electronics purchase for rule testing
    (21086, '2026-09-20 10:14:37', '2026-09-20', 156.78,6256.17, 100620, NULL,   'POS', 'THE HOME DEPOT #6584',            'Debit Card Purchase - Home Improvement',  92609200001, 1015, 1, 5200, 'TRC240920001', 0, '2026-09-20 10:14:37'),
    (21086, '2026-09-20 13:41:09', '2026-09-20', 32.50, 6223.67, 100620, NULL,   'POS', 'DOMINOS PIZZA 4471',              'Debit Card Purchase - Dining',            92609200002, 1016, 1, 5812, 'TRC240920002', 0, '2026-09-20 13:41:09'),
    (21086, '2026-09-20 16:22:55', '2026-09-20', 249.99,5973.68, 100620, NULL,   'POS', 'BEST BUY #1128',                  'Debit Card Purchase - Electronics',       92609200003, 1017, 1, 5732, 'TRC240920003', 0, '2026-09-20 16:22:55'),

    -- 2026-09-21 (Sunday)
    (21086, '2026-09-21 12:08:41', '2026-09-21', 94.21, 5879.47, 100721, NULL,   'POS', 'WHOLE FOODS MKT #10245',          'Debit Card Purchase - Groceries',         92609210001, 1018, 1, 5411, 'TRC240921001', 0, '2026-09-21 12:08:41'),
    (21086, '2026-09-21 17:36:20', '2026-09-21', 48.33, 5831.14, 100721, NULL,   'POS', 'CHEVRON 0384729',                 'Debit Card Purchase - Gas',               92609210002, 1019, 1, 5541, 'TRC240921002', 0, '2026-09-21 17:36:20'),

    -- 2026-09-22 (Monday)
    (21086, '2026-09-22 06:30:00', '2026-09-22', 425.00,5406.14, 100822, NULL,   'ACH', 'AUTO LOAN PAYMENT',               'Scheduled Loan Payment',                  92609220001, 1020, 1, NULL, 'TRC240922001', 0, '2026-09-22 06:30:00'),
    (21086, '2026-09-22 08:12:00', '2026-09-22', 10.99, 5395.15, 100822, NULL,   'ACH', 'SPOTIFY USA',                     'Subscription - Monthly',                  92609220002, 1021, 1, 4899, 'TRC240922002', 0, '2026-09-22 08:12:00'),
    (21086, '2026-09-22 16:48:27', '2026-09-22', 18.44, 5376.71, 100822, NULL,   'POS', 'CVS/PHARMACY #8841',              'Debit Card Purchase - Pharmacy',          92609220003, 1022, 1, 5912, 'TRC240922003', 0, '2026-09-22 16:48:27'),
    (21086, '2026-09-22 18:05:11', '2026-09-22', 215.00,5161.71, 100822, 1042,   'CHK', 'CHECK #1042',                     'Check Paid - Home Services',              92609220004, 1023, 1, NULL, 'TRC240922004', 0, '2026-09-22 18:05:11'),

    -- 2026-09-23 (Tuesday - today)
    (21086, '2026-09-23 08:05:33', '2026-09-23', 4.89,  5156.82, 100923, NULL,   'POS', 'DUTCH BROS COFFEE',               'Debit Card Purchase - Coffee',            92609230001, 1024, 1, 5814, 'TRC240923001', 0, '2026-09-23 08:05:33'),
    (21086, '2026-09-23 12:31:47', '2026-09-23', 16.75, 5140.07, 100923, NULL,   'POS', 'POTBELLY SANDWICH',               'Debit Card Purchase - Lunch',             92609230002, 1025, 1, 5812, 'TRC240923002', 0, '2026-09-23 12:31:47'),
    (21086, '2026-09-23 18:44:02', '2026-09-23', 24.60, 5115.47, 100923, NULL,   'POS', 'UBER *TRIP',                      'Debit Card Purchase - Rideshare',         92609230003, 1026, 1, 4121, 'TRC240923003', 0, '2026-09-23 18:44:02'),
    (21086, '2026-09-23 21:19:28', '2026-09-23', 212.35,4903.12, 100923, NULL,   'POS', 'APPLE STORE R185',                'Debit Card Purchase - Electronics',       92609230004, 1027, 1, 5732, 'TRC240923004', 0, '2026-09-23 21:19:28'),

    -- 2026-09-24 (Wednesday - tomorrow)
    (21086, '2026-09-24 06:00:00', '2026-09-24', 49.99, 4853.13, 100924, NULL,   'ACH', 'PLANET FITNESS EFT',              'Scheduled Gym Membership',                92609240001, 1028, 1, 7997, 'TRC240924001', 0, '2026-09-24 06:00:00'),
    (21086, '2026-09-24 09:15:00', '2026-09-24', 25.00, 4878.13, 100924, NULL,   'ACH', 'AMAZON REFUND',                   'Merchant Refund Credit',                  92609240002, 1029, 0, 5399, 'TRC240924002', 0, '2026-09-24 09:15:00'),
    (21086, '2026-09-24 17:52:19', '2026-09-24', 78.32, 4799.81, 100924, NULL,   'POS', 'HEB GROCERY #567',                'Debit Card Purchase - Groceries',         92609240003, 1030, 1, 5411, 'TRC240924003', 0, '2026-09-24 17:52:19'),
    (21086, '2026-09-24 20:08:44', '2026-09-24', 285.00,4514.81, 100924, NULL,   'POS', 'COSTCO WHSE #1234',               'Debit Card Purchase - Warehouse Club',    92609240004, 1031, 1, 5300, 'TRC240924004', 0, '2026-09-24 20:08:44');

COMMIT TRAN;

-- Verify inserted rows
SELECT
    [ID],
    [PostingDate],
    [EffectiveDate],
    [Amount],
    [Balance],
    [Debit],
    [TransactionCode],
    [GeneralDescription],
    [MerchantClassificationCode],
    [CoreSequence]
FROM [core].[Transactions]
WHERE [AccountID] = 21086
  AND [PostingDate] >= '2026-09-16'
  AND [PostingDate] <  '2026-09-25'
ORDER BY [PostingDate], [PostingSequence];

GO
