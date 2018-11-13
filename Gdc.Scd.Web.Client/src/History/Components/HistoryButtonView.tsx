import * as React from "react";
import { HistoryWindowView, HistoryWindowViewProps, Position } from "./HistoryWindowView";
import { HistoryValuesGridViewProps } from "./HistoryValuesGridView";
import { Container, Button } from "@extjs/ext-react";

export interface HistoryButtonViewProps extends HistoryValuesGridViewProps {
    isEnabled: boolean
    flex: number
    windowPosition?: Position
}

export interface HistoryButtonViewState {
    isVisibleWindow: boolean
}

export class HistoryButtonView extends React.Component<HistoryButtonViewProps, HistoryButtonViewState> {
    constructor(props) {
        super(props);

        this.state = {
            isVisibleWindow: false
        }
    }

    public render() {
        const { dataLoadUrl, isEnabled, flex, windowPosition } = this.props;

        const windowProps: HistoryWindowViewProps = {
            dataLoadUrl,
            isVisible: this.state.isVisibleWindow,
            onClose: this.closeHistoryWindow,
            position: windowPosition
        };

        return (
            <Container flex={flex} layout={{type: 'vbox', align: 'center'}} >
                <Button 
                    text="History" 
                    disabled={!isEnabled}
                    handler={this.showHistoryWindow}
                />

                {
                    this.state.isVisibleWindow &&
                    <HistoryWindowView {...windowProps} />
                }
            </Container>
        );
    }

    private showHistoryWindow= () => {
        this.setState({
            isVisibleWindow: true
        });
    }

    private closeHistoryWindow = () => {
        this.setState({ 
            isVisibleWindow: false 
        });
    }
}