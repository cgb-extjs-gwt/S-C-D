import { connect } from "react-redux";
import { ApprovalValuesProps, ApprovalValuesActions, ApprovalValuesViewComponent, DetailsProps } from "./ApprovalValuesViewComponent";
import { CommonState } from "../../Layout/States/AppStates";
import * as CostApprovalService from "../Services/CostApprovalService"
import { API_URL, buildMvcUrl } from "../../Common/Services/Ajax";
import { NamedId } from "../../Common/States/CommonStates";
import { buildGetApproveBundleDetailUrl } from "../Services/CostApprovalService";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { getDependecyColumns, getInputLevelColumns, buildNameColumnInfo } from "../../Common/Helpers/ColumnInfoHelper";
import { ApprovalBundle } from "../States/ApprovalBundle";
import { getDependencies } from "../../Common/Helpers/MetaHelper";

export interface ApprovalValuesContainerProps {
    approvalBundle: ApprovalBundle
    onHandled?()
}

export const ApprovalValuesContainerComponent = 
    connect<ApprovalValuesProps, ApprovalValuesActions, ApprovalValuesContainerProps, CommonState>(
        (state, { approvalBundle }) => {
            const meta = state.app.appMetaData;

            let columns: ColumnInfo[];
            let dataLoadUrl: string;
            let details: DetailsProps;
            
            if (meta) {
                dataLoadUrl = buildGetApproveBundleDetailUrl(approvalBundle.id);

                const costBlock = meta.costBlocks.find(item => item.id === approvalBundle.costBlock.id);
                const dependencies = getDependencies(costBlock);

                const dependencyColumns = getDependecyColumns(dependencies);
                const inputLevelColumns = getInputLevelColumns(costBlock);
                const otherColumns = [
                    { title: 'Value', dataIndex: 'Value', type: ColumnType.Simple },
                    { title: 'Period error', dataIndex: `IsPeriodError`, type: ColumnType.Checkbox },
                    { title: 'Country group error', dataIndex: `IsRegionError`, type: ColumnType.Checkbox }
                ];

                columns = [
                    buildNameColumnInfo(approvalBundle.inputLevel),
                    ...dependencyColumns,
                    ...otherColumns
                ]

                details = {
                    columns: [
                        ...inputLevelColumns,
                        ...dependencyColumns,
                        ...otherColumns
                    ],
                    buildDataLoadUrl: data => {
                        const costBlockFilter = {};

                        for (const dependency of dependencies) {
                            costBlockFilter[dependency.id] = [
                                data[`${dependency.id}Id`]
                            ];
                        }

                        return buildGetApproveBundleDetailUrl(approvalBundle.id, data.HistoryValueId, costBlockFilter);
                    }
                };
            } else {
                columns = [];
            }

            return <ApprovalValuesProps>{
                dataLoadUrl,
                columns,
                id: approvalBundle.costBlock.id.toString(),
                details,
                message: approvalBundle.qualityGateErrorExplanation
            }
        },
        (dispatch, { approvalBundle, onHandled }) => ({
            onApprove: () => {
                CostApprovalService.approve(approvalBundle.id)
                onHandled && onHandled();
            },
            onSendBackToRequestor: message => {
                CostApprovalService.reject(approvalBundle.id, message);
                onHandled && onHandled();
            }
        })
    )(ApprovalValuesViewComponent)