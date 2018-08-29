import { connect } from "react-redux";
import { ApprovalValuesProps, ApprovalValuesActions, ApprovalValuesViewComponent } from "./ApprovalValuesViewComponent";
import { CommonState } from "../../Layout/States/AppStates";
import * as CostApprovalService from "../Services/CostApprovalService"
import { API_URL, buildMvcUrl } from "../../Common/Services/Ajax";
import { NamedId } from "../../Common/States/CommonStates";
import { buildGetApproveBundleDetailUrl } from "../Services/CostApprovalService";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { getDependecyColumnsFromMeta } from "../../Common/Helpers/ColumnInfoHelper";

export interface ApprovalValuesContainerProps {
    bundleId: number
    costBlockId: string
    onHandled?()
}

export const ApprovalValuesContainerComponent = 
    connect<ApprovalValuesProps, ApprovalValuesActions, ApprovalValuesContainerProps, CommonState>(
        (state, { bundleId, costBlockId }) => {
            const meta = state.app.appMetaData;

            let columns: ColumnInfo[];
            let dataLoadUrl: string;
            
            if (meta) {
                dataLoadUrl = buildGetApproveBundleDetailUrl(bundleId);

                columns = [
                    { title: 'InputLevel', dataIndex: 'InputLevelName', type: ColumnType.Simple },
                    ...getDependecyColumnsFromMeta(meta, costBlockId),
                    { title: 'Value', dataIndex: 'Value', type: ColumnType.Simple }
                ]
            } else {
                columns = [];
            }

            return <ApprovalValuesProps>{
                dataLoadUrl,
                columns,
                id: bundleId.toString()
            }
        },
        (dispatch, { bundleId, costBlockId, onHandled }) => ({
            onApprove: () => {
                CostApprovalService.approve(bundleId)
                onHandled && onHandled();
            },
            onSendBackToRequestor: message => {
                CostApprovalService.reject(bundleId, message);
                onHandled && onHandled();
            }
        })
    )(ApprovalValuesViewComponent)