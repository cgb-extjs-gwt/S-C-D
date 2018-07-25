import { CostEditorState } from "../../CostEditor/States/CostEditorStates";

export interface AppState {
    isLoading: boolean
    error: any
    currentPage: {
        id: string
        title: string
    }
}

export interface CommonState {
    app: AppState
    pages: {
        costEditor: CostEditorState
    }
}