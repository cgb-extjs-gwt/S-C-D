import { CostEditorState } from "../../CostEditor/States/CostEditorStates";
import { CostMetaData } from "../../Common/States/CostMetaStates";
import { ApprovalCostElementsLayoutState } from "../../CostApproval/States/ApprovalCostElementsLayoutState";
import { OwnApprovalCostElementsLayoutState } from "../../CostApproval/States/OwnApprovalCostElementsLayoutState";

export interface AppState {
    isLoading: boolean
    error: any
    currentPage: {
        id: string
        title: string
    }
    appMetaData: CostMetaData
}

export interface CommonState {
    app: AppState
    pages: {
        costEditor: CostEditorState,
        costApproval: ApprovalCostElementsLayoutState,
        ownCostApproval: OwnApprovalCostElementsLayoutState
    }
}