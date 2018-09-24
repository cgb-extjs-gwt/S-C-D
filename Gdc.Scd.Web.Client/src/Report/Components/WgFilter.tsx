import { DictFilter, DictFilterProps } from "./DictFilter";

export class WgFilter extends DictFilter {
    public constructor(props: DictFilterProps) {
        super({
            ...props,
            getItems: x => x.getWG()
        });
    }
}