import { CostEditorState } from "../../CostEditor/States/CostEditorStates";
import { CostMetaData } from "../../Common/States/CostMetaStates";
import { ApprovalCostElementsLayoutState } from "../../CostApproval/States/ApprovalCostElementsLayoutState";
import { OwnApprovalCostElementsLayoutState } from "../../CostApproval/States/OwnApprovalCostElementsLayoutState";
import { TableViewState } from "../../TableView/States/TableViewState";
import { NamedId } from "../../Common/States/CommonStates";
import { CostImportState } from "../../CostImport/States/CostImportState";

export interface Role {
    name: string
    isGlobal: boolean
    country: NamedId
    permissions: string[]
}

export interface AppState {
    currentPage: {
        id: string
    }
    appMetaData: CostMetaData
    userRoles: Role[]
    appVersion: string
}

export interface AppData {
    meta: CostMetaData
    userRoles: Role[]
    appVersion: string
}

export interface CommonState {
    app: AppState
    pages: {
        costEditor: CostEditorState,
        costApproval: ApprovalCostElementsLayoutState,
        ownCostApproval: OwnApprovalCostElementsLayoutState,
        tableView: TableViewState
        costImport: CostImportState
    }
}