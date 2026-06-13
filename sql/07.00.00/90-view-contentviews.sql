-- Apply key 07.00.00/90 | Hood v7.0.0 - HoodContentViews reporting view (idempotent DROP/CREATE). Embedded; applied by hood-schema (DbUp) in LogicalName order.
IF EXISTS(select * FROM sys.views where name = 'HoodContentViews') DROP VIEW HoodContentViews
GO
CREATE VIEW HoodContentViews AS
SELECT  
	HoodContent.Id,
	HoodContent.AllowComments,
	HoodContent.AuthorId,
	HoodContent.Body,
	HoodContent.ContentType,
	HoodContent.CreatedBy,
	HoodContent.CreatedOn,
	HoodContent.Excerpt,
	HoodContent.FeaturedImageJson,
	HoodContent.LastEditedBy,
	HoodContent.LastEditedOn,
	HoodContent.ParentId,
	HoodContent.[Public],
	HoodContent.PublishDate,
	HoodContent.ShareCount,
	HoodContent.Slug,
	HoodContent.[Status],
	HoodContent.Title,
	HoodContent.Views,
	HoodContent.Featured,
	HoodContent.ShareImageJson,
	AspNetUsers.FirstName,
	AspNetUsers.LastName,
	AspNetUsers.AvatarJson,
	AspNetUsers.Anonymous,
	AspNetUsers.DisplayName,
	AspNetUsers.Email as AuthorEmail, 
	AspNetUsers.UserVars as AuthorVars
FROM
	HoodContent JOIN
	AspNetUsers ON HoodContent.AuthorId = AspNetUsers.Id
GO