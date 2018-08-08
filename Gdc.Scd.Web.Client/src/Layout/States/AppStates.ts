import { CostEditorState } from "../../CostEditor/States/CostEditorStates";
import { CostBlockMeta, CostMetaData } from "../../Common/States/CostMetaStates";
import { BundleFilterStates } from "../../CostApproval/States/BundleFilterStates";
import { applyFilters } from "../../CostEditor/Actions/CostBlockActions";

export interface AppState {
    isLoading: boolean
    error: any
    currentPage: {
        id: string
        title: string
    }
    appMetaData: CostMetaData
}

export interface BudleFilter{
    filter: BundleFilterStates,
    applyFilter: BundleFilterStates
}

export interface CommonState {
    app: AppState
    pages: {
        costEditor: CostEditorState,
        costApproval: BudleFilter
    }
}