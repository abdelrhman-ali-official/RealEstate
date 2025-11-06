-- =======================================================
-- Chat Data Cleanup Script for Real Estate Application
-- =======================================================
-- This script cleans up invalid foreign key references in ChatRooms
-- and related entities before applying foreign key constraints
-- =======================================================

PRINT 'Starting Chat Data Cleanup...';

-- Step 1: Remove ChatMessageReactions for orphaned messages
PRINT 'Step 1: Cleaning ChatMessageReactions for orphaned messages...';
DELETE FROM ChatMessageReactions 
WHERE MessageId IN (
    SELECT cm.Id 
    FROM ChatMessages cm 
    LEFT JOIN ChatRooms cr ON cm.ChatRoomId = cr.Id 
    WHERE cr.Id IS NULL
);
PRINT '✓ ChatMessageReactions cleaned';

-- Step 2: Remove ChatMessageStatuses for orphaned messages  
PRINT 'Step 2: Cleaning ChatMessageStatuses for orphaned messages...';
DELETE FROM ChatMessageStatuses 
WHERE MessageId IN (
    SELECT cm.Id 
    FROM ChatMessages cm 
    LEFT JOIN ChatRooms cr ON cm.ChatRoomId = cr.Id 
    WHERE cr.Id IS NULL
);
PRINT '✓ ChatMessageStatuses cleaned';

-- Step 3: Remove orphaned ChatMessages
PRINT 'Step 3: Removing orphaned ChatMessages...';
DELETE FROM ChatMessages 
WHERE ChatRoomId NOT IN (SELECT Id FROM ChatRooms);
PRINT '✓ Orphaned ChatMessages removed';

-- Step 4: Remove ChatRooms with invalid User1Id references
PRINT 'Step 4: Removing ChatRooms with invalid User1Id...';
DELETE FROM ChatRooms 
WHERE User1Id NOT IN (SELECT Id FROM AspNetUsers);
PRINT '✓ ChatRooms with invalid User1Id removed';

-- Step 5: Remove ChatRooms with invalid User2Id references
PRINT 'Step 5: Removing ChatRooms with invalid User2Id...';
DELETE FROM ChatRooms 
WHERE User2Id NOT IN (SELECT Id FROM AspNetUsers);
PRINT '✓ ChatRooms with invalid User2Id removed';

-- Step 6: Remove ChatRooms with invalid PropertyId references
PRINT 'Step 6: Removing ChatRooms with invalid PropertyId...';
DELETE FROM ChatRooms 
WHERE PropertyId NOT IN (SELECT Id FROM Properties);
PRINT '✓ ChatRooms with invalid PropertyId removed';

-- Step 7: Final cleanup - Remove any remaining orphaned records
PRINT 'Step 7: Final cleanup of remaining orphaned records...';

-- Clean up ChatMessageReactions again
DELETE FROM ChatMessageReactions 
WHERE MessageId IN (
    SELECT cm.Id 
    FROM ChatMessages cm 
    LEFT JOIN ChatRooms cr ON cm.ChatRoomId = cr.Id 
    WHERE cr.Id IS NULL
);

-- Clean up ChatMessageStatuses again
DELETE FROM ChatMessageStatuses 
WHERE MessageId IN (
    SELECT cm.Id 
    FROM ChatMessages cm 
    LEFT JOIN ChatRooms cr ON cm.ChatRoomId = cr.Id 
    WHERE cr.Id IS NULL
);

-- Clean up ChatMessages again
DELETE FROM ChatMessages 
WHERE ChatRoomId NOT IN (SELECT Id FROM ChatRooms);

PRINT '✓ Final cleanup completed';

-- Step 8: Display cleanup summary
PRINT '=======================================================';
PRINT 'CLEANUP SUMMARY:';
SELECT 
    'ChatRooms' AS TableName,
    COUNT(*) AS RemainingRecords
FROM ChatRooms
UNION ALL
SELECT 
    'ChatMessages' AS TableName,
    COUNT(*) AS RemainingRecords
FROM ChatMessages
UNION ALL
SELECT 
    'ChatMessageReactions' AS TableName,
    COUNT(*) AS RemainingRecords
FROM ChatMessageReactions
UNION ALL
SELECT 
    'ChatMessageStatuses' AS TableName,
    COUNT(*) AS RemainingRecords
FROM ChatMessageStatuses;

PRINT '=======================================================';
PRINT '✅ Chat data cleanup completed successfully!';
PRINT 'You can now apply the EF Core migration with foreign key constraints.';
PRINT '=======================================================';