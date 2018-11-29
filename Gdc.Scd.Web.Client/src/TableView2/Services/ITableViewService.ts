import { TableViewInfo, QualityGateResultSet } from "../../TableView/States/TableViewState";
import { CostMetaData } from "../../Common/States/CostMetaStates";
import { TableViewRecord } from "../../TableView/States/TableViewRecord";
import { ApprovalOption } from "../../QualityGate/States/ApprovalOption";

export interface ITableViewService {
    getMeta(): Promise<CostMetaData>;

    getSchema(): Promise<TableViewInfo>;

    getUrl(): string;

    updateRecords(records: TableViewRecord[], approvalOption: ApprovalOption): Promise<QualityGateResultSet>;
}