import { connect } from "react-redux";
import { buildNameColumnInfo, getInputLevelColumns } from "../../Common/Helpers/ColumnInfoHelper";
import { getDependency, getCostElementByAppMeta, getCostBlock, getCostElement, getLastInputLevel } from "../../Common/Helpers/MetaHelper";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { CommonState } from "../../Layout/States/AppStates";
import { buildGetApproveBundleDetailUrl } from "../Services/CostApprovalService";
import { ApprovalBundle } from "../States/ApprovalBundle";
import { ApprovalValuesProps, ApprovalValuesViewComponent, DetailsProps } from "./ApprovalValuesViewComponent";

export interface ApprovalValuesContainerProps {
    approvalBundle: ApprovalBundle
    isCheckColumnsVisible: boolean
}

export const ApprovalValuesContainerComponent =
    connect<ApprovalValuesProps, {}, ApprovalValuesContainerProps, CommonState>(
        ({ app: { appMetaData } }, { approvalBundle, isCheckColumnsVisible }) => {
            const costBlock = getCostBlock(appMetaData, approvalBundle.costBlock.id);
            const costElement = getCostElement(costBlock, approvalBundle.costElement.id);
            const lastInputLevel = getLastInputLevel(costElement);

            return <ApprovalValuesProps>{
                id: approvalBundle.costBlock.id,
                message: approvalBundle.qualityGateErrorExplanation,
                hideCheckColumns: !isCheckColumnsVisible,
                costElement: costElement,
                inputLevelId: approvalBundle.inputLevel.id,
                dataLoadUrl: buildGetApproveBundleDetailUrl(approvalBundle.id),
                details: {
                    inputLevelId: lastInputLevel.id,
                    buildDataLoadUrl: data => {
                        const costBlockFilter = {};
    
                        if (costElement.dependency) {
                            costBlockFilter[costElement.dependency.id] = [
                                data[`${costElement.dependency.id}Id`]
                            ]
                        }
    
                        return buildGetApproveBundleDetailUrl(approvalBundle.id, data.HistoryValueId, costBlockFilter);
                    }
                },
            }
        }
    )(ApprovalValuesViewComponent)