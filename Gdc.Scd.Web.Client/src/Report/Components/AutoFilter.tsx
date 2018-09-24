import { Button, Container, NumberField, Panel, PanelProps, TextField } from "@extjs/ext-react";
import * as React from "react";
import { AutoFilterModel } from "../Model/AutoFilterModel";
import { AutoFilterType } from "../Model/AutoFilterType";

export interface AutoFilterPanelProps extends PanelProps {
    filter: AutoFilterModel[];
    onSearch(filter: any): void;
}

export class AutoFilter extends React.Component<AutoFilterPanelProps, any> {

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        var filter = this.props.filter || [];

        return (
            <Panel {...this.props} margin="0 0 5px 0" padding="4px 20px 7px 20px">

                <Container
                    ref="fields"
                    margin="10px 0"
                    defaults={{
                        maxWidth: '200px',
                        clearable: 'true'
                    }}>

                    {filter.map((v, i) => {

                        switch (v.type) {

                            case AutoFilterType.NUMBER:
                                return (
                                    <NumberField key={i} ref={v.name} name={v.name} label={v.text} value={v.value} />
                                );

                            case AutoFilterType.TEXT:
                            default:
                                return (
                                    <TextField key={i} ref={v.name} name={v.name} label={v.text} value={v.value} />
                                );

                        }
                    })}

                </Container>

                <Button text="Search" ui="action" width="85px" handler={this.onSearch} margin="20px auto" />

            </Panel>
        );
    }

    public getModel(): any {
        let result = {};
        let filter = this.props.filter;

        if (filter) {
            for (let i = 0, item; item = filter[i]; i++) {

                let f = this.refs[item.name] as any;

                if (f.getValue()) {
                    result[item.name] = f.getValue();
                }
            }
        }

        return result;
    }

    private init() {
        this.onSearch = this.onSearch.bind(this);
    }

    private onSearch() {
        let handler = this.props.onSearch;
        if (handler) {
            handler(this.getModel());
        }
    }
}