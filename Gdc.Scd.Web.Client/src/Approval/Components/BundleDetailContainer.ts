import { connect } from "react-redux";
import { buildNameColumnInfo, getInputLevelColumns } from "../../Common/Helpers/ColumnInfoHelper";
import { getDependency, getCostElementByAppMeta, getCostBlock, getCostElement, getLastInputLevel } from "../../Common/Helpers/MetaHelper";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { CommonState } from "../../Layout/States/AppStates";
import { BundleDetailProps, BundleDetailView } from "./BundleDetailView";
import { buildGetApproveBundleDetailUrl } from "../Services/ApprovalService";
import { Bundle } from "../States/ApprovalState";

export interface ApprovalValuesContainerProps {
    approvalBundle: Bundle
    isCheckColumnsVisible: boolean
}

export const BundleDetailContainer =
    connect<BundleDetailProps, {}, ApprovalValuesContainerProps, CommonState>(
        ({ app: { appMetaData } }, { approvalBundle, isCheckColumnsVisible }) => {
            const costBlock = getCostBlock(appMetaData, approvalBundle.costBlock.id);
            const costElement = getCostElement(costBlock, approvalBundle.costElement.id);
            const lastInputLevel = getLastInputLevel(costElement);

            return <BundleDetailProps>{
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

                        for (const key of Object.keys(data.coordinates)) {
                            costBlockFilter[key] = data.coordinates[key].map(item => item.id);
                        }
    
                        return buildGetApproveBundleDetailUrl(approvalBundle.id, data.historyValueId, costBlockFilter);
                    }
                },
            }
        }
    )(BundleDetailView)