import { KeyValue } from "../interfaces/KeyValue";

export class UserStatistics {
    totalUsers: number;
    totalAdmins: string;
    days: KeyValue<string, number>[];
    months: KeyValue<string, number>[];
}
