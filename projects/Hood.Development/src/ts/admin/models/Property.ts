import { KeyValue } from "../../interfaces/KeyValue";

export class PropertyStatistics {
    totalProperties: number;
    totalPublished: number

    days: KeyValue<string, number>[];
    months: KeyValue<string, number>[];
    publishDays: KeyValue<string, number>[];
    publishMonths: KeyValue<string, number>[];

}
