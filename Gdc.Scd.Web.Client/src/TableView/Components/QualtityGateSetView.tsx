import { Container, TabPanel } from "@extjs/ext-react";
import * as React from "react";
import { WgInputLevel } from "../../Common/Constants/MetaConstants";
import { CostElementMeta } from "../../Common/States/CostMetaStates";
import { QualityGateGrid } from "../../QualityGate/Components/QualityGateGrid";
import { QualityGateToolbar, QualityGateToolbarActions } from "../../QualityGate/Components/QualityGateToolbar";
import { BundleDetailGroup } from "../../QualityGate/States/QualityGateResult";

export interface QualtityGateTab {
    title: string
    key: string
    costElement: CostElementMeta
    errors: BundleDetailGroup[]
}

export interface QualtityGateSetProps extends QualityGateToolbarActions {
    tabs?: QualtityGateTab[]
}

export class QualtityGateSetView extends React.Component<QualtityGateSetProps> {
    public render() {
        const { tabs, onSave, onCancel } = this.props;

        return (
            <Container layout="vbox">
                <TabPanel tabBar={{ layout: { pack: 'left' } }} flex={10}>
                    {tabs.map(this.createTab)}
                </TabPanel>

                <QualityGateToolbar onSave={onSave} onCancel={onCancel} flex={1} />
            </Container>
        );
    }

    private createTab({ title, costElement, errors, key }: QualtityGateTab) {
        return <Container key={key} title={title}>
            <QualityGateGrid
                minHeight="450"
                costElement={costElement}
                storeConfig={{ data: errors }}
                inputLevelId={WgInputLevel}
                flex={1}
            />
        </Container>
    }

}
