import { connect } from "react-redux";
import { ApprovalValuesProps, ApprovalValuesViewComponent, DetailsProps } from "./ApprovalValuesViewComponent";
import { CommonState } from "../../Layout/States/AppStates";
import * as CostApprovalService from "../Services/CostApprovalService"
import { API_URL, buildMvcUrl } from "../../Common/Services/Ajax";
import { NamedId } from "../../Common/States/CommonStates";
import { buildGetApproveBundleDetailUrl } from "../Services/CostApprovalService";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { getInputLevelColumns, buildNameColumnInfo } from "../../Common/Helpers/ColumnInfoHelper";
import { ApprovalBundle } from "../States/ApprovalBundle";
import { getDependency } from "../../Common/Helpers/MetaHelper";

export interface ApprovalValuesContainerProps {
    approvalBundle: ApprovalBundle
    isCheckColumnsVisible: boolean
}

export const ApprovalValuesContainerComponent = 
    connect<ApprovalValuesProps, {}, ApprovalValuesContainerProps, CommonState>(
        (state, { approvalBundle, isCheckColumnsVisible }) => {
            const meta = state.app.appMetaData;

            let columns: ColumnInfo[];
            let dataLoadUrl: string;
            let details: DetailsProps;
            
            if (meta) {
                dataLoadUrl = buildGetApproveBundleDetailUrl(approvalBundle.id);

                const costBlock = meta.costBlocks.find(item => item.id === approvalBundle.costBlock.id);

                const otherColumns: ColumnInfo[] = []; 
                const dependency = getDependency(costBlock, approvalBundle.costElement.id);
                
                if (dependency) {
                    const dependencyColumn = buildNameColumnInfo(dependency);

                    otherColumns.push(dependencyColumn);
                }

                otherColumns.push({ title: 'Value', dataIndex: 'Value', type: ColumnType.Text });

                if(isCheckColumnsVisible) {
                    otherColumns.push({ title: 'Period error', dataIndex: `IsPeriodError`, type: ColumnType.CheckBox });
                    otherColumns.push({ title: 'Country group error', dataIndex: `IsRegionError`, type: ColumnType.CheckBox });
                }

                columns = [
                    buildNameColumnInfo(approvalBundle.inputLevel),
                    ...otherColumns
                ]

                const inputLevelColumns = getInputLevelColumns(costBlock, approvalBundle.costElement.id);

                details = {
                    columns: [
                        ...inputLevelColumns,
                        ...otherColumns
                    ],
                    buildDataLoadUrl: data => {
                        const costBlockFilter = {};

                        if (dependency) {
                            costBlockFilter[dependency.id] = [
                                data[`${dependency.id}Id`]
                            ]
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
        }
    )(ApprovalValuesViewComponent)