-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SobekCM_Get_Item_Aggregation_Dark_Counts]

AS

BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select ia.Code as code,i.Dark as dark, count(*) as count 
		
		from SobekCM_Item as i 
		join SobekCM_Item_Aggregation_Item_Link as il on i.ItemID=il.itemid 
		join SobekCM_Item_Aggregation as ia on ia.AggregationID = il.AggregationID 
		
		where Code!='all' and Dark=1
		
		group by ia.code, Dark 
		
		order by ia.Code;

END
GO
