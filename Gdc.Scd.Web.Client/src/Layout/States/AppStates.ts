import { CostEditorState } from "../../CostEditor/States/CostEditorStates";
import { CostMetaData } from "../../Common/States/CostMetaStates";
import { TableViewState } from "../../TableView/States/TableViewState";
import { NamedId } from "../../Common/States/CommonStates";
import { CostImportState } from "../../CostImport/States/CostImportState";
import { ApprovalCostElementsLayoutState } from "../../Approval/States/ApprovalState";
import { OwnApprovalCostElementsLayoutState } from "../../CostOwnApproval/States/OwnApprovalState";

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
}

export interface AppData {
    meta: CostMetaData
    userRoles: Role[]
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