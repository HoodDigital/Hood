import { KeyValue } from "../../interfaces/KeyValue";

export class ContentStatistics {
    totalPosts: number;
    totalPublished: number;

    days: KeyValue<string, number>[];
    months: KeyValue<string, number>[];
    publishDays: KeyValue<string, number>[];
    publishMonths: KeyValue<string, number>[];

    byType: ContentTypeStatistic[];
}

export class ContentTypeStatistic {
    type: ContentType;
    total: number;
    typeName: string;
}

export class ContentType {
    BaseName: string;
    CachedByType: boolean;
    CustomFields: any;
    CustomFieldsJson: string;
    Description: string;
    Enabled: boolean;
    ExcerptName: string;
    Gallery: boolean;
    HasPage: boolean;
    HideAuthor: boolean;
    Icon: string;
    IsPublic: boolean;
    IsUnknown: boolean;
    MetaTitle: string;
    MultiLineExcerpt: boolean;
    NoImage: string;
    RichTextExcerpt: boolean;
    Search: string;
    ShowBanner: boolean;
    ShowCategories: boolean;
    ShowDesigner: boolean;
    ShowEditor: boolean;
    ShowImage: boolean;
    ShowPreview: boolean;
    Slug: string;
    TemplateFolder: string;
    Templates: boolean;
    Title: string;
    TitleName: string;
    Type: string;
    TypeName: string;
    TypeNamePlural: string;
    UrlFormatting: string;
}
