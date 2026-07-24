import type { IMagicCardDetail, IMagicCardSearchResponse } from '../Interfaces/APIResponses/IMagicCard';
import type { IOnePieceCardDetail, IOnePieceCardSearchResponse } from '../Interfaces/APIResponses/IOnePieceCard';
import type { IPokemonCardDetail, IPokemonCardSearchResponse } from '../Interfaces/APIResponses/IPokemonCard';
import { apiPath, requestApi } from './apiClient';
import type { ApiResult } from './types';

const magicCardsPath = '/api/tcg/magic/cards';
const onePieceCardsPath = '/api/tcg/one-piece/cards';
const pokemonCardsPath = '/api/tcg/pokemon/cards';

export function searchOnePieceCards(query: string, pageSize = 12): Promise<ApiResult<IOnePieceCardSearchResponse>> {
  return requestApi<IOnePieceCardSearchResponse>(apiPath(onePieceCardsPath, '', { query, pageSize }), {
    failureMessage: 'One Piece card search failed',
    connectionMessage: 'Unable to connect to the TCG API.',
  });
}

export function getOnePieceCard(id: string): Promise<ApiResult<IOnePieceCardDetail>> {
  return requestApi<IOnePieceCardDetail>(apiPath(onePieceCardsPath, `/${encodeURIComponent(id)}`), {
    failureMessage: 'One Piece card detail request failed',
    connectionMessage: 'Unable to connect to the TCG API.',
  });
}

export function searchMagicCards(query: string, pageSize = 12): Promise<ApiResult<IMagicCardSearchResponse>> {
  return requestApi<IMagicCardSearchResponse>(apiPath(magicCardsPath, '', { query, pageSize }), {
    failureMessage: 'Magic card search failed',
    connectionMessage: 'Unable to connect to the TCG API.',
  });
}

export function getMagicCard(id: string): Promise<ApiResult<IMagicCardDetail>> {
  return requestApi<IMagicCardDetail>(apiPath(magicCardsPath, `/${encodeURIComponent(id)}`), {
    failureMessage: 'Magic card detail request failed',
    connectionMessage: 'Unable to connect to the TCG API.',
  });
}

export function searchPokemonCards(query: string, pageSize = 12): Promise<ApiResult<IPokemonCardSearchResponse>> {
  return requestApi<IPokemonCardSearchResponse>(apiPath(pokemonCardsPath, '', { query, pageSize }), {
    failureMessage: 'Pokemon card search failed',
    connectionMessage: 'Unable to connect to the TCG API.',
  });
}

export function getPokemonCard(id: string): Promise<ApiResult<IPokemonCardDetail>> {
  return requestApi<IPokemonCardDetail>(apiPath(pokemonCardsPath, `/${encodeURIComponent(id)}`), {
    failureMessage: 'Pokemon card detail request failed',
    connectionMessage: 'Unable to connect to the TCG API.',
  });
}
