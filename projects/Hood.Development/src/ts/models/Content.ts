import { KeyValue } from "../interfaces/KeyValue";

export declare interface ContentStatistics {
    totalPosts: number;
    totalPublished: number;

    days: KeyValue<string, number>[];
    months: KeyValue<string, number>[];
    publishDays: KeyValue<string, number>[];
    publishMonths: KeyValue<string, number>[];

    byType: ContentTypeStatistic[];
}

export declare interface ContentTypeStatistic {
    type: ContentType;
    total: number;
    name: string;
}

export declare interface ContentType {
    baseName: string;
    cachedByType: boolean;
    customFields: any;
    customFieldsJson: string;
    description: string;
    enabled: boolean;
    excerptName: string;
    gallery: boolean;
    hasPage: boolean;
    hideAuthor: boolean;
    icon: string;
    isPublic: boolean;
    isUnknown: boolean;
    metaTitle: string;
    multiLineExcerpt: boolean;
    noImage: string;
    richTextExcerpt: boolean;
    search: string;
    showBanner: boolean;
    showCategories: boolean;
    showDesigner: boolean;
    showEditor: boolean;
    showImage: boolean;
    showPreview: boolean;
    slug: string;
    templateFolder: string;
    templates: boolean;
    title: string;
    titleName: string;
    type: string;
    typeName: string;
    typeNamePlural: string;
    urlFormatting: string;
}
