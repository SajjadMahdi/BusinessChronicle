using BusinessChronicle.Abstractions.Contracts;
using BusinessChronicle.Abstractions.Models;
using BusinessChronicle.Abstractions.Options;
using BusinessChronicle.DependencyInjection;
using BusinessChronicle.Samples.Shared.Models;
using BusinessChronicle.Samples.Shared.Presentation;
using BusinessChronicle.Samples.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessChronicle.Samples.WinForms;

internal sealed class PropertyPortfolioForm : Form
{
    private readonly PropertyUnitCatalog _catalog;
    private readonly IChronicleStore _store;

    private readonly Label _statusLabel = new() { AutoSize = true, Font = new Font("Segoe UI Semibold", 11F) };
    private readonly Label _titleLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 14F, FontStyle.Bold), MaximumSize = new Size(320, 0) };
    private readonly Label _tenantLabel = new() { AutoSize = true, ForeColor = Color.FromArgb(100, 116, 139) };
    private readonly Label _rentLabel = new() { AutoSize = true, ForeColor = Color.FromArgb(100, 116, 139) };
    private readonly ProgressBar _progressBar = new() { Height = 12, Maximum = 100 };
    private readonly Label _progressLabel = new() { AutoSize = true, ForeColor = Color.FromArgb(100, 116, 139) };
    private readonly Button _actionButton = new() { Height = 42, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 75, 122), ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 10F) };
    private readonly Label _completeLabel = new() { AutoSize = true, ForeColor = Color.FromArgb(21, 128, 61), Text = "Unit occupied — full lease history saved.", Visible = false };
    private readonly ListView _auditList = new() { View = View.Details, FullRowSelect = true, GridLines = false, BorderStyle = BorderStyle.None, HeaderStyle = ColumnHeaderStyle.Nonclickable };

    public PropertyPortfolioForm(PropertyUnitCatalog catalog, IChronicleStore store)
    {
        _catalog = catalog;
        _store = store;

        Text = "Chronicle ERP — Property Portfolio";
        BackColor = Color.FromArgb(238, 242, 247);
        MinimumSize = new Size(980, 640);
        ClientSize = new Size(1040, 680);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10F);

        _actionButton.FlatAppearance.BorderSize = 0;
        _actionButton.Click += async (_, _) => await AdvanceAsync().ConfigureAwait(true);

        _auditList.Columns.Add("When", 170);
        _auditList.Columns.Add("Event", 360);
        _auditList.Columns.Add("Field change", 220);

        BuildLayout();
        Shown += async (_, _) => await InitializeAsync().ConfigureAwait(true);
    }

    private void BuildLayout()
    {
        Panel header = new()
        {
            Dock = DockStyle.Top,
            Height = 88,
            Padding = new Padding(20, 16, 20, 0),
        };
        header.Controls.Add(new Label
        {
            Text = "COMMERCIAL REAL ESTATE · ERP DESKTOP",
            ForeColor = Color.FromArgb(13, 148, 136),
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 0),
        });
        header.Controls.Add(new Label
        {
            Text = "Chronicle ERP — Property Portfolio",
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 18),
        });
        header.Controls.Add(new Label
        {
            Text = "Lease workflow demo for Riverside Business Park — Unit 4B",
            ForeColor = Color.FromArgb(100, 116, 139),
            AutoSize = true,
            Location = new Point(0, 50),
        });

        Panel leftCard = CreateCard(new Point(20, 108), new Size(340, 520));
        Panel rightCard = CreateCard(new Point(380, 108), new Size(640, 520));

        _statusLabel.Location = new Point(20, 16);
        _titleLabel.Location = new Point(20, 44);
        _tenantLabel.Location = new Point(20, 110);
        _rentLabel.Location = new Point(20, 132);
        _progressBar.Location = new Point(20, 170);
        _progressBar.Width = 300;
        _progressLabel.Location = new Point(20, 188);
        _actionButton.Location = new Point(20, 230);
        _actionButton.Width = 300;
        _completeLabel.Location = new Point(20, 230);
        _completeLabel.MaximumSize = new Size(300, 0);

        leftCard.Controls.AddRange([
            _statusLabel, _titleLabel, _tenantLabel, _rentLabel,
            _progressBar, _progressLabel, _actionButton, _completeLabel,
        ]);

        Label auditTitle = new()
        {
            Text = "Lease lifecycle history",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 16),
        };
        Label auditEyebrow = new()
        {
            Text = "AUDIT TRAIL",
            ForeColor = Color.FromArgb(13, 148, 136),
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 0),
        };
        _auditList.Location = new Point(16, 48);
        _auditList.Size = new Size(608, 452);
        rightCard.Controls.AddRange([auditEyebrow, auditTitle, _auditList]);

        Label footer = new()
        {
            Text = "Simulate ERP workflow: publish listing → record offer → sign lease → mark move-in.",
            ForeColor = Color.FromArgb(100, 116, 139),
            AutoSize = true,
            Location = new Point(20, 640),
        };

        Controls.Add(header);
        Controls.Add(leftCard);
        Controls.Add(rightCard);
        Controls.Add(footer);
    }

    private static Panel CreateCard(Point location, Size size) => new()
    {
        Location = location,
        Size = size,
        BackColor = Color.White,
        BorderStyle = BorderStyle.FixedSingle,
    };

    private async Task InitializeAsync()
    {
        await _catalog.EnsureSeededAsync().ConfigureAwait(true);
        await RefreshAsync().ConfigureAwait(true);
    }

    private async Task AdvanceAsync()
    {
        PropertyUnit? unit = _catalog.GetUnit(PropertyUnitCatalog.DefaultUnitId);
        if (unit is null)
        {
            return;
        }

        _actionButton.Enabled = false;
        try
        {
            _ = await _catalog.AdvanceUnitAsync(unit.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        finally
        {
            _actionButton.Enabled = unit.CanAdvance;
        }
    }

    private async Task RefreshAsync()
    {
        PropertyUnit? unit = _catalog.GetUnit(PropertyUnitCatalog.DefaultUnitId);
        if (unit is null)
        {
            return;
        }

        _statusLabel.Text = $"{unit.StatusEmoji} {unit.StatusLabel}";
        _titleLabel.Text = unit.DisplayTitle;
        _tenantLabel.Text = $"Prospect: {unit.ProspectTenant}";
        _rentLabel.Text = $"Rent: {unit.RentLabel}";
        _progressBar.Value = Math.Clamp(unit.ProgressPercent, 0, 100);
        _progressLabel.Text = $"Lease pipeline {unit.ProgressPercent}% complete";

        bool canAdvance = unit.CanAdvance && unit.NextActionLabel is not null;
        _actionButton.Visible = canAdvance;
        _actionButton.Text = unit.NextActionLabel ?? "Next step";
        _actionButton.Enabled = canAdvance;
        _completeLabel.Visible = !canAdvance;

        EntityReference entity = unit.ToEntityReference();
        var timelineResult = await _store.Query
            .GetTimelineAsync(entity, new TimelineQueryOptions { MaxResults = 20, Descending = false }, CancellationToken.None)
            .ConfigureAwait(true);

        _auditList.Items.Clear();
        if (timelineResult.IsSuccess)
        {
            foreach (TimelineEntry entry in timelineResult.Value!)
            {
                PropertyAuditLine line = PropertyAuditLine.FromEntry(entry);
                _auditList.Items.Add(new ListViewItem([line.TimeLabel, line.Headline, line.FieldChange]));
            }
        }
    }
}
