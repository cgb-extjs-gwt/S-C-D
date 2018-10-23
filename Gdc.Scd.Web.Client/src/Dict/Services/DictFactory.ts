import { DictService } from "../../Dict/Services/DictService";
import { IDictService } from "../../Dict/Services/IDictService";

export class DictFactory {

    public static getDictService(): IDictService {
        return new DictService();
    }

}