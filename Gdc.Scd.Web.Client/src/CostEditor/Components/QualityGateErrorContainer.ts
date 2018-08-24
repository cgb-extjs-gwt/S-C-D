import { connect } from "react-redux";
import { QualityGateErrorProps, QualityGateErrorActions } from "./QualityGateErrorView";
import { CommonState } from "../../Layout/States/AppStates";
import { getDependecyColumnsFromMeta } from "../../Common/Helpers/ColumnInfoHelper";
import { ColumnInfo } from "../../Common/States/ColumnInfo";
import { saveEditItemsToServer } from "../Actions/CostBlockActions";

export interface QualityGateErrorContainerProps {
    costBlockId: string
    errors: {[key: string]: any}[]
}

export const QualityGateErrorContainer = 
    connect<QualityGateErrorProps, QualityGateErrorActions, QualityGateErrorContainerProps, CommonState>(
        (state, { costBlockId, errors }) => {
            const meta = state.app.appMetaData;
            let columns: ColumnInfo[] = [];

            if (meta) {
                columns = [
                    { title: 'Wg', dataIndex: `WarrantyGroupName` },
                    ...getDependecyColumnsFromMeta(meta, costBlockId),
                ];
            }

            return <QualityGateErrorProps> {
                columns,
                errors
            };
        },
        (dispatch, { costBlockId }) => ({
            //onSave: (explanationMessage) => dispatch(saveEditItemsToServer(costBlockId))
        })
    )