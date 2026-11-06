namespace Crm.Backend.Persistence.Entities;

/// <summary>Pipeline stages for an opportunity, stored as their string name.</summary>
public enum OpportunityStage
{
    New,
    Qualified,
    Proposal,
    Negotiation,
    Won,
    Lost,
}
