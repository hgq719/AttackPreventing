IF EXISTS
(
    SELECT name 
    FROM SYSOBJECTS
    WHERE TYPE ='U' AND NAME ='t_Action_Report'
)
BEGIN
ALTER TABLE t_Action_Report ADD IfCreateWhiteLimit int NOT NULL DEFAULT(0);
END;