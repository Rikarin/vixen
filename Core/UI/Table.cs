using ImGuiNET;

namespace Rin.Core.UI;

public class Table<T> : View {
    readonly T[] data;
    readonly TableColumn<T>[] columns;

    public Table(T[] data, TableColumn<T>[] columns) {
        this.data = data;
        this.columns = columns;
    }

    public override void Render() {
        var num = columns.Length;
        var config = GetConfiguration<TableStyle>() ?? TableStyle.Default;

        if (ImGui.BeginTable($"###{ViewContext.GetId()}", num)) {
            if (config.HasHeader) {
                foreach (var column in columns) {
                    ImGui.TableSetupColumn(column.Header);
                }
                ImGui.TableHeadersRow();
            }
            
            foreach (var entry in data) {
                ImGui.TableNextRow();
                foreach (var column in columns) {
                    ImGui.TableNextColumn();
                    if (column.Formatter != null) {
                        ImGui.Text(column.Formatter(entry));
                    } else {
                        column.Content!.Invoke(entry).Render();
                    }
                }
            }

            ImGui.EndTable();
        }

        base.Render();
    }
}
