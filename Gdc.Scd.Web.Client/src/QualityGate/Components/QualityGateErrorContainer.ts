import { connect } from "react-redux";
import { QualityGateErrorProps, QualityGateErrorView } from "./QualityGateErrorView";
import { CommonState } from "../../Layout/States/AppStates";
import { getDependecyColumnFromMeta } from "../../Common/Helpers/ColumnInfoHelper";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { BundleDetailGroup } from "../States/QualityGateResult";
import { getCostElementByAppMeta } from "../../Common/Helpers/MetaHelper";
import { QualityGateToolbarActions } from "./QualityGateToolbar";

export interface QualityGateErrorContainerProps extends QualityGateToolbarActions {
    costBlockId: string
    costElementId: string
    errors?: BundleDetailGroup[]
}

export const QualityGateErrorContainer = 
    connect<QualityGateErrorProps, QualityGateToolbarActions, QualityGateErrorContainerProps, CommonState>(
        ({ app: { appMetaData } }, { errors, costBlockId, costElementId }) => (<QualityGateErrorProps>{ 
            errors,
            costElement: getCostElementByAppMeta(appMetaData, costBlockId, costElementId)
        }),
        (dispatch, { onSave, onCancel }) => ({
            onSave: explanationMessage => onSave && onSave(explanationMessage),
            onCancel: () => onCancel && onCancel()
        })
    )(QualityGateErrorView)