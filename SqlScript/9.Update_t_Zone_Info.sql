IF EXISTS
(
    SELECT name 
    FROM SYSOBJECTS
    WHERE TYPE ='U' AND NAME ='t_Zone_Info'
)
BEGIN
UPDATE t_Zone_Info
SET AuthEmail = 'cloudflareapidep@comm100.com', AuthKey = 'Bh4yzL0DRq5WFhawU3FmdD6OjQ5DLY5tmg3gSbLJDObu8rGR4yKvvngn8pGDhn2d'
WHERE lOWER(ZoneName) IN ('comm100.com', 'comm100.chat');
END;