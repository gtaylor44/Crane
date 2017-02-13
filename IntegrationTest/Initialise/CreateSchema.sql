USE [SprocMapperTest]

DROP TABLE President
DROP TABLE PresidentAssistant

CREATE TABLE President
(
Id INTEGER NOT NULL,
FirstName NVARCHAR(128) NULL,
LastName NVARCHAR(128) NULL,
Fans INTEGER NULL,
IsHonest BIT NULL,
PRIMARY KEY(Id)
)

CREATE TABLE PresidentAssistant
(
Id INTEGER NOT NULL,
PresidentId INTEGER NOT NULL,
FirstName NVARCHAR(128),
LastName NVARCHAR(128),
PRIMARY KEY(Id),
FOREIGN KEY (PresidentId) REFERENCES President(Id)
)

GO
/****** Object:  StoredProcedure [dbo].[GetPresidentList]    Script Date: 9/02/2017 9:13:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetPresidentList] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT p.Id, p.FirstName, p.LastName, p.Fans, p.IsHonest, pa.PresidentId, pa.FirstName as AssistantFirstName, pa.LastName as AssistantLastName
	FROM dbo.President p
	LEFT JOIN dbo.PresidentAssistant pa
	ON p.Id = pa.PresidentId
END

GO
/****** Object:  StoredProcedure [dbo].[GetPresidentList]    Script Date: 9/02/2017 9:13:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetPresidentList2] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT *
	FROM dbo.President p
END
