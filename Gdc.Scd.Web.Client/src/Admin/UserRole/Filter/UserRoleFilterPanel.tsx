import * as React from "react";
import { ComboBoxField, CheckBoxField, Container, Button, Panel, PanelProps } from "@extjs/ext-react";
import { UserRoleFilterModel } from "./UserRoleFilterModel";

export interface UserRoleFilterPanelProps extends PanelProps {
    storeUser?,
    roles: any[],
    countries: any[],
    onSearch(filter: UserRoleFilterModel): void
}

export class UserRoleFilterPanel extends React.Component<UserRoleFilterPanelProps, any> {

    private user: ComboBoxField;

    private country: ComboBoxField;

    private role: ComboBoxField;

    private email: ComboBoxField;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        const { storeUser, roles, countries } = this.props;
        let storeEmail = storeUser.data.items.slice().sort(this.compare);

        return (
            <Panel title="Filter By" {...this.props} margin="0 0 5px 0" padding="4px 20px 7px 20px" width="350px">

                <Container margin="10px 0"
                    defaults={{
                        valueField: 'id',
                        displayField: 'name',
                        queryMode: 'local',
                        clearable: 'true',
                        style: 'font-size:10px'
                    }}
                >
                    <ComboBoxField ref="user" label="User:" store={storeUser} />
                    <ComboBoxField ref="email" label="E-mail:" options={storeEmail} displayField='email'/>
                    <ComboBoxField ref="role" label="Role:" options={roles} />
                    <ComboBoxField ref="country" label="Country:" options={countries} />
                </Container>

                <Button text="Search" ui="action" width="100px" handler={this.onSearch} margin="20px auto" />

            </Panel>
        );
    }

    public componentDidMount() {
        this.user = this.refs.user as ComboBoxField;
        this.email = this.refs.email as ComboBoxField;
        this.role = this.refs.role as ComboBoxField;
        this.country = this.refs.country as ComboBoxField;
    }

    public getModel(): UserRoleFilterModel {
        return {
            user: this.getSelected(this.user) || this.getSelected(this.email),
            role: this.getSelected(this.role),
            country: this.getSelected(this.country)
        };
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

    private getSelected(cb: ComboBoxField): string {
        let result: string = null;
        let selected = (cb as any).getSelection();
        if (selected) {
            result = selected.data.id;
        }
        return result;
    }

    private compare = (a, b) => {
        if (a.data.email > b.data.email) {
            return 1;
        }
        if (a.data.email < b.data.email) {
            return -1;
        }
        return 0;
    }
}