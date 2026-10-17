namespace Whyland.Domain.Common.Primitives;

/// <summary>
/// Marker interface for all persisted entities.
/// Used by cross-cutting concerns (error tracking, interceptors) to reference
/// an entity type without depending on its generic key.
/// </summary>
public interface IEntity;