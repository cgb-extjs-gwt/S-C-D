import { CostBlockState } from "./CostBlockStates";
import { Action } from "redux";
import { NamedId, SelectList } from "../../Common/States/CommonStates";
import { CostBlockMeta } from "../../Common/States/CostMetaStates";

export interface ApplicationState {
    id: string
    costBlocks: SelectList<CostBlockState>
}

export interface CostEditorState {
    applications: SelectList<ApplicationState>
}