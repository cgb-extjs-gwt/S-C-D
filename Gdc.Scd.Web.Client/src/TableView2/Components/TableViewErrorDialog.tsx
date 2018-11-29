import * as React from "react";
import { Dialog, DialogProps } from "@extjs/ext-react";
import { QualityGateResultSet } from "../../TableView/States/TableViewState";
import { QualtityGateSetView, QualtityGateTab } from "../../TableView/Components/QualtityGateSetView";

export interface TableViewErrorDialogProps extends DialogProps {
    //onOk(message: string): void;
    //onCancel(): void;
}

export class TableViewErrorDialog extends React.Component<TableViewErrorDialogProps, any> {

    private dlg: Dialog & any;

    public state = {
        showDialog: false,
        tabs: []
    };

    public constructor(props: TableViewErrorDialogProps) {
        super(props);
        this.init();
    }

    public render() {
        return <Dialog
            {...this.props}
            ref={x => this.dlg = x}
            displayed={this.state.showDialog}
            closable
            closeAction="hide"
            draggable={false}
            maximizable
            resizable={{
                dynamic: true,
                edges: 'all'
            }}
            minHeight="50%"
            minWidth="60%"
            layout="fit"
        >
            <QualtityGateSetView tabs={this.state.tabs} />
        </Dialog>;
    }

    public show() {
        this.dlg.show();
    }

    public hide() {
        this.setState({ showDialog: false });
    }

    public setModel(m: QualityGateResultSet) {
        let tabs: QualtityGateTab[] = [];

        if (m && m.hasErrors) {
            //const { recordInfo } = info;

            //for (const item of m.items) {
            //    if (item.qualityGateResult.hasErrors) {
            //        const { applicationId, costBlockId, costElementId } = item.costElementIdentifier;

            //        const fieldInfos = recordInfo.data.filter(
            //            fieldInfo =>
            //                fieldInfo.metaId == costBlockId &&
            //                fieldInfo.fieldName == costElementId
            //        );

            //        const costBlock = getCostBlock(appMetaData, costBlockId);
            //        const costElement = getCostElement(costBlock, costElementId);

            //        tabs.push(...fieldInfos.map(fieldInfo => <QualtityGateTab>{
            //            key: `${applicationId}_${costBlockId}_${costElementId}`,
            //                title: `${costBlock.name} ${costElement.name}`,
            //            costElement,
            //            errors: item.qualityGateResult.errors
            //        }));
            //    }
            //}
        }

        this.setState({ tabs: tabs });
    }

    private init() {
    }

}