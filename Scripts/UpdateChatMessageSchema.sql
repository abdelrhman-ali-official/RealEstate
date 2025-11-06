-- Script to update ChatMessages table with missing columns for message status functionality
-- This will fix the database schema mismatch causing the 500 error

USE [StoreDB] -- Replace with your actual database name
GO

-- Check if columns exist before adding them to avoid errors
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[ChatMessages]') AND name = 'DeliveredAt')
BEGIN
    ALTER TABLE [dbo].[ChatMessages] 
    ADD [DeliveredAt] datetime2 NULL
    PRINT 'Added DeliveredAt column to ChatMessages'
END
ELSE
BEGIN
    PRINT 'DeliveredAt column already exists in ChatMessages'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[ChatMessages]') AND name = 'ReadAt')
BEGIN
    ALTER TABLE [dbo].[ChatMessages] 
    ADD [ReadAt] datetime2 NULL
    PRINT 'Added ReadAt column to ChatMessages'
END
ELSE
BEGIN
    PRINT 'ReadAt column already exists in ChatMessages'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[ChatMessages]') AND name = 'IsDelivered')
BEGIN
    ALTER TABLE [dbo].[ChatMessages] 
    ADD [IsDelivered] bit NOT NULL DEFAULT 0
    PRINT 'Added IsDelivered column to ChatMessages'
END
ELSE
BEGIN
    PRINT 'IsDelivered column already exists in ChatMessages'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[ChatMessages]') AND name = 'IsRead')
BEGIN
    ALTER TABLE [dbo].[ChatMessages] 
    ADD [IsRead] bit NOT NULL DEFAULT 0
    PRINT 'Added IsRead column to ChatMessages'
END
ELSE
BEGIN
    PRINT 'IsRead column already exists in ChatMessages'
END

-- Verify the columns were added
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ChatMessages'
AND COLUMN_NAME IN ('DeliveredAt', 'ReadAt', 'IsDelivered', 'IsRead')
ORDER BY COLUMN_NAME

PRINT 'ChatMessages table schema update completed successfully!'