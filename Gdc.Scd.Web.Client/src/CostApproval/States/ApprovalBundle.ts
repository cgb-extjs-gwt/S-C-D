import { NamedId } from "../../Common/States/CommonStates";

export interface ApprovalBundle {
    id: number
    editDate: string
    editUser: NamedId
    editItemCount: number
    isDifferentValues: boolean
    application: NamedId
    regionInput: NamedId
    costBlock: NamedId
    costElement: NamedId
    inputLevel: NamedId
}