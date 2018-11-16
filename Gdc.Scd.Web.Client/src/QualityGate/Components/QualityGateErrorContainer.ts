import { connect } from "react-redux";
import { QualityGateErrorProps, QualityGateErrorActions, QualityGateErrorView } from "./QualityGateErrorView";
import { CommonState } from "../../Layout/States/AppStates";
import { getDependecyColumnFromMeta } from "../../Common/Helpers/ColumnInfoHelper";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { BundleDetailGroup } from "../States/QualityGateResult";
import { getCostElementByAppMeta } from "../../Common/Helpers/MetaHelper";

export interface QualityGateErrorContainerProps extends QualityGateErrorActions {
    costBlockId: string
    costElementId: string
    errors?: BundleDetailGroup[]
}

export const QualityGateErrorContainer = 
    connect<QualityGateErrorProps, QualityGateErrorActions, QualityGateErrorContainerProps, CommonState>(
        ({ app: { appMetaData } }, { errors, costBlockId, costElementId }) => (<QualityGateErrorProps>{ 
            errors,
            costElement: getCostElementByAppMeta(appMetaData, costBlockId, costElementId)
        }),
        (dispatch, { onSave, onCancel }) => ({
            onSave: explanationMessage => onSave && onSave(explanationMessage),
            onCancel: () => onCancel && onCancel()
        })
    )(QualityGateErrorView)