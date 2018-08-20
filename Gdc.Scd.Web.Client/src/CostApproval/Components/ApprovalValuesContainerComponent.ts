import { connect } from "react-redux";
import { ApprovalValuesProps, ColumnInfo, ApprovalValuesActions, ApprovalValuesViewComponent } from "./ApprovalValuesViewComponent";
import { CommonState } from "../../Layout/States/AppStates";
import * as CostApprovalService from "../Services/CostApprovalService"
import { API_URL, buildMvcUrl } from "../../Common/Services/Ajax";
import { NamedId } from "../../Common/States/CommonStates";
import { buildGetHistoryValueTableUrl } from "../Services/CostApprovalService";

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
                dataLoadUrl = buildGetHistoryValueTableUrl(bundleId);

                const costBlock = meta.costBlocks.find(item => item.id === costBlockId);
                const dependencyColumnsMap = new Map<string, ColumnInfo>();
                
                for (const costElement of costBlock.costElements) {
                    if (costElement.dependency && !dependencyColumnsMap.has(costElement.dependency.name)) {
                        dependencyColumnsMap.set(
                            costElement.dependency.name, 
                            <ColumnInfo>{
                                title: costElement.dependency.name,
                                dataIndex: `${costElement.dependency.id}Name`
                            });
                    }
                }

                columns = [
                    { title: 'InputLevel', dataIndex: 'InputLevelName' },
                    ...Array.from(dependencyColumnsMap.values()),
                    { title: 'Value', dataIndex: 'Value' }
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