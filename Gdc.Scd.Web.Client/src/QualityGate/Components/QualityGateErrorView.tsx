import * as React from "react";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { Container, FormPanel, TextField, Toolbar, Button, Grid, Column, GridCell, CheckColumn } from "@extjs/ext-react";
import { DynamicGrid } from "../../Common/Components/DynamicGrid";
import { BundleDetailGroup } from "../States/QualityGateResult";
import { Model } from "../../Common/States/ExtStates";
import { CostElementMeta } from "../../Common/States/CostMetaStates";
import { QualityGateGridProps, QualityGateGrid } from "./QualityGateGrid";
import { WgInputLevel } from "../../Common/Constants/MetaConstants";
import { QualityGateToolbarActions, QualityGateToolbar } from "./QualityGateToolbar";

export interface QualityGateErrorProps extends QualityGateToolbarActions {
    errors?: BundleDetailGroup[]
    costElement: CostElementMeta
}

export class QualityGateErrorView extends React.Component<QualityGateErrorProps> {
    render() {
        const { onSave, onCancel, costElement, errors } = this.props;
        return (
            <Container layout="vbox" scrollable={true}>
                <QualityGateGrid costElement={costElement} storeConfig={{ data: errors }} inputLevelId={WgInputLevel} flex={10}/>
                <QualityGateToolbar onSave={onSave} onCancel={onCancel} flex={1}/>
            </Container>
        );
    }
}